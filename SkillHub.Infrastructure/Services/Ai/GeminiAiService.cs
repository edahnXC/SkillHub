using System;

using System.Collections.Concurrent;

using System.Collections.Generic;

using System.Linq;

using System.Net.Http;

using System.Text;

using System.Text.Json;

using System.Text.Json.Serialization;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Options;

using SkillHub.Application.Features.Ai.DTOs;

using SkillHub.Application.Features.Ai.Interfaces;

using SkillHub.Infrastructure.Persistence.Context;



namespace SkillHub.Infrastructure.Services.Ai;



public class GeminiAiService : IAiChatService

{

    private readonly GeminiSettings _settings;

    private readonly HttpClient _httpClient;

    private readonly ApplicationDbContext _context;

    private readonly ILogger<GeminiAiService> _logger;



    // In-memory guest quota tracking (GuestSessionId/IP -> request count)

    private static readonly ConcurrentDictionary<string, int> _guestQuotaTracker = new();

    private const int MaxGuestQuestions = 5;



    public GeminiAiService(

        IOptions<GeminiSettings> options,

        HttpClient httpClient,

        ApplicationDbContext context,

        ILogger<GeminiAiService> logger)

    {

        _settings = options.Value;

        _httpClient = httpClient;

        _context = context;

        _logger = logger;

    }



    public async Task<AiChatResponseDto> SendMessageAsync(AiChatRequestDto request, long? userId, string clientIp)

    {

        if (string.IsNullOrWhiteSpace(request.Message))

        {

            return new AiChatResponseDto

            {

                Reply = "I am listening! How can I assist with your learning journey on SkillHub today?",

                Suggestions = GetQuickSuggestions(request.ContextUrl)

            };

        }



        // Quota Guard for Unregistered Guests

        if (!userId.HasValue)

        {

            var trackerKey = !string.IsNullOrWhiteSpace(request.GuestSessionId)

                ? request.GuestSessionId.Trim()

                : clientIp;



            var currentCount = _guestQuotaTracker.AddOrUpdate(trackerKey, 1, (_, count) => count + 1);

            var remaining = Math.Max(0, MaxGuestQuestions - currentCount);



            if (currentCount > MaxGuestQuestions)

            {

                return new AiChatResponseDto

                {

                    Reply = "🌟 **You've reached your free guest limit (5 questions)!**\n\nWe love your curiosity! To continue chatting with your **SkillHub AI Learning Copilot**, asking unlimited programming questions, and tracking your courses, please **[Sign In](/login)** or **[Create a Free Account](/register)**.",

                    Suggestions = new List<string> { "Sign In to SkillHub", "Create Free Account", "Browse Courses" },

                    RemainingGuestQuota = 0,

                    IsGuestLimitReached = true

                };

            }



            // Continue with remaining count

            return await ExecuteChatAsync(request, userId, remaining);

        }



        return await ExecuteChatAsync(request, userId, null);

    }



    private async Task<AiChatResponseDto> ExecuteChatAsync(AiChatRequestDto request, long? userId, int? remainingGuestQuota)

    {

        // If an API Key is configured, attempt live call to Google Gemini 1.5/2.0 Flash

        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))

        {

            try

            {

                var geminiReply = await CallGoogleGeminiApiAsync(request);

                if (!string.IsNullOrWhiteSpace(geminiReply))

                {

                    return new AiChatResponseDto

                    {

                        Reply = geminiReply,

                        Suggestions = GenerateDynamicSuggestions(request.Message, request.ContextUrl),

                        RemainingGuestQuota = remainingGuestQuota,

                        IsGuestLimitReached = false,

                        ModelUsed = _settings.Model

                    };

                }

            }

            catch (Exception ex)

            {

                _logger.LogWarning(ex, "Gemini API call encountered an error. Falling back to local SkillHub Smart Guide.");

            }

        }



        // SkillHub Built-in Smart Fallback Engine (Runs without API key or offline)

        var fallbackReply = await GenerateSmartFallbackReplyAsync(request);

        return new AiChatResponseDto

        {

            Reply = fallbackReply,

            Suggestions = GenerateDynamicSuggestions(request.Message, request.ContextUrl),

            RemainingGuestQuota = remainingGuestQuota,

            IsGuestLimitReached = false,

            ModelUsed = "SkillHub Smart Copilot (Local Guide)"

        };

    }



    private async Task<string> CallGoogleGeminiApiAsync(AiChatRequestDto request)

    {

        var model = string.IsNullOrWhiteSpace(_settings.Model) ? "gemini-1.5-flash" : _settings.Model;

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_settings.ApiKey}";



        var systemPrompt = await BuildFactualSystemPromptAsync();



        var contents = new List<object>();



        // Previous conversation turns

        if (request.History != null)

        {

            foreach (var h in request.History.TakeLast(6))

            {

                contents.Add(new

                {

                    role = h.Role == "model" ? "model" : "user",

                    parts = new object[] { new { text = h.Text } }

                });

            }

        }



        // Current user message

        var contextHint = !string.IsNullOrWhiteSpace(request.ContextUrl)

            ? $" [User Current Route: {request.ContextUrl}]"

            : "";



        contents.Add(new

        {

            role = "user",

            parts = new object[] { new { text = request.Message + contextHint } }

        });



        var requestBody = new

        {

            system_instruction = new

            {

                parts = new object[] { new { text = systemPrompt } }

            },

            contents,

            generationConfig = new

            {

                temperature = _settings.Temperature,

                maxOutputTokens = _settings.MaxOutputTokens

            }

        };



        var jsonContent = new StringContent(

            JsonSerializer.Serialize(requestBody),

            Encoding.UTF8,

            "application/json");



        var response = await _httpClient.PostAsync(url, jsonContent);

        if (!response.IsSuccessStatusCode)

        {

            var err = await response.Content.ReadAsStringAsync();

            _logger.LogWarning("Gemini API returned error {StatusCode}: {Error}", response.StatusCode, err);

            return string.Empty;

        }



        var jsonResponse = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(jsonResponse);



        if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&

            candidates.GetArrayLength() > 0 &&

            candidates[0].TryGetProperty("content", out var content) &&

            content.TryGetProperty("parts", out var parts) &&

            parts.GetArrayLength() > 0 &&

            parts[0].TryGetProperty("text", out var textProp))

        {

            return textProp.GetString() ?? string.Empty;

        }



        return string.Empty;

    }



    private async Task<string> GenerateSmartFallbackReplyAsync(AiChatRequestDto request)
    {
        var msg = request.Message.ToLower().Trim();

        // 1. Aptitude: Trains & Platforms (Shortcut Method)
        if (msg.Contains("train") || msg.Contains("platform") || (msg.Contains("speed") && (msg.Contains("distance") || msg.Contains("km/h") || msg.Contains("m/s"))))
        {
            return "### 🚂 Speed, Time & Distance: Trains & Platforms (Shortcut Method)\n\n" +
                   "**1. Golden Conversion Formula:**\n" +
                   "- To convert **km/h to m/s**: Multiply by `5/18`\n" +
                   "- To convert **m/s to km/h**: Multiply by `18/5`\n\n" +
                   "**2. Key Formulas & Rules:**\n" +
                   "- **Train crossing a pole or stationary person**: Distance = `Length of Train`\n" +
                   "- **Train crossing a platform, bridge, or tunnel**: Distance = `Length of Train + Length of Platform`\n" +
                   "- **Relative Speed**:\n" +
                   "  - Trains moving in **opposite directions**: `Relative Speed = S1 + S2`\n" +
                   "  - Trains moving in **same direction**: `Relative Speed = |S1 - S2|`\n\n" +
                   "**Solved Example:**\n" +
                   "A train 200m long running at 72 km/h crosses a 300m platform. Find the time taken:\n" +
                   "1. Convert Speed to m/s: `72 * (5/18) = 20 m/s`\n" +
                   "2. Total Distance: `200m + 300m = 500m`\n" +
                   "3. Time = `Distance / Speed = 500 / 20 = 25 seconds`!";
        }

        // 2. Aptitude: Time & Work (LCM Unit Method)
        if (msg.Contains("time and work") || msg.Contains("time & work") || 
            (msg.Contains("work") && (msg.Contains("days") || msg.Contains("together") || msg.Contains("efficiency") || msg.Contains("alone") || msg.Contains("lcm"))))
        {
            return "### ⏱️ Time & Work: The LCM Unit Method (Fastest Shortcut)\n\n" +
                   "Never waste time with fraction addition! Use the **LCM Unit Method**:\n\n" +
                   "**Step-by-Step Technique:**\n" +
                   "1. Find the LCM of the individual days — this represents the **Total Units of Work**.\n" +
                   "2. Divide Total Work by each person's days to find their **Daily Efficiency (Units/Day)**.\n" +
                   "3. Sum their daily efficiencies to get their combined working rate.\n" +
                   "4. `Total Days Needed = Total Work Units / Combined Efficiency`.\n\n" +
                   "**Solved Example:**\n" +
                   "A completes a job in 10 days, B in 15 days. Working together, how many days will they take?\n" +
                   "- **Total Work = LCM(10, 15) = 30 units**\n" +
                   "- A's efficiency = `30 / 10 = 3 units/day`\n" +
                   "- B's efficiency = `30 / 15 = 2 units/day`\n" +
                   "- Combined efficiency = `3 + 2 = 5 units/day`\n" +
                   "- Days together = `30 / 5 = 6 days`!";
        }

        // 3. Aptitude: Simple & Compound Interest (Shortcuts & Formulas)
        if (msg.Contains("interest") || msg.Contains("compound") || msg.Contains("simple interest") || msg.Contains("si & ci") || msg.Contains("si and ci"))
        {
            return "### 💰 Simple & Compound Interest (Formulas & Shortcuts)\n\n" +
                   "**1. Simple Interest (SI):**\n" +
                   "- Formula: `SI = (P * R * T) / 100`\n" +
                   "- Interest remains constant every year.\n\n" +
                   "**2. Compound Interest (CI):**\n" +
                   "- Formula: `Amount = P * (1 + R/100)^T`\n" +
                   "- `CI = Amount - P`\n\n" +
                   "**3. The 2-Year CI vs SI Difference Shortcut:**\n" +
                   "- `Difference (D) = P * (R / 100)^2`\n\n" +
                   "**Solved Example:**\n" +
                   "Principal = ₹10,000, Rate = 10% per annum, Time = 2 years.\n" +
                   "- Difference = `10,000 * (10 / 100)^2 = 10,000 * 0.01 = ₹100`!";
        }

        // 4. Career Roadmaps
        if (msg.Contains("roadmap") || msg.Contains("career") || msg.Contains("learning path") || msg.Contains("how to become"))
        {
            return "### 🚀 SkillHub Career Roadmaps for Indian Software Engineers\n\n" +
                   "1. **Enterprise Full-Stack .NET & Angular Developer**:\n" +
                   "   - C# 12 & .NET 8 Fundamentals\n" +
                   "   - ASP.NET Core Web APIs & Clean Architecture\n" +
                   "   - Entity Framework Core & SQL Server Performance\n" +
                   "   - Angular 17+ (Signals, Standalone Architecture & RxJS)\n" +
                   "   - Docker, Microservices & Azure Deployment\n\n" +
                   "2. **Data Science & Generative AI Specialist**:\n" +
                   "   - Python Programming & Data Manipulation (Pandas, NumPy)\n" +
                   "   - Machine Learning Algorithms (Scikit-Learn)\n" +
                   "   - Deep Learning & Neural Networks (PyTorch)\n" +
                   "   - LLM Orchestration, Prompt Engineering & RAG Systems\n\n" +
                   "Explore all structured courses on our **[Courses](/courses)** catalog!";
        }

        // 5. Technical: Dependency Injection & .NET
        if (msg.Contains("dependency injection") || msg.Contains("di") || (msg.Contains("lifecycle") && msg.Contains("service")) || (msg.Contains(".net") && msg.Contains("service")))
        {
            return "### 🧱 Dependency Injection in ASP.NET Core (.NET 8)\n\n" +
                   "Dependency Injection is a core first-class citizen in ASP.NET Core. Service lifetimes are configured in `Program.cs`:\n\n" +
                   "- **Transient** (`AddTransient`): A new instance is created every time it is requested. Best for lightweight, stateless operations.\n" +
                   "- **Scoped** (`AddScoped`): A single instance is created per client request (HTTP cycle). Best for Entity Framework `DbContext` and business services.\n" +
                   "- **Singleton** (`AddSingleton`): Created once on startup and reused across all subsequent requests. Best for caches or application-wide state.\n\n" +
                   "```csharp\nbuilder.Services.AddScoped<ICourseService, CourseService>();\n```";
        }

        // 6. Technical: Angular & Signals
        if (msg.Contains("angular") || msg.Contains("signals") || msg.Contains("standalone"))
        {
            return "### ⚡ Modern Angular (v17+ Standalone & Signals)\n\n" +
                   "SkillHub courses teach cutting-edge Angular architecture:\n\n" +
                   "- **Standalone Components**: Clean modularity without bulky `NgModule` imports.\n" +
                   "- **Signals**: Reactive primitives (`signal()`, `computed()`, `effect()`) for fine-grained reactivity and ultra-fast change detection.\n" +
                   "- **Built-in Control Flow**: Modern `@if`, `@for`, and `@switch` syntax replaces legacy structural directives.\n\n" +
                   "```typescript\n@Component({\n  standalone: true,\n  imports: [CommonModule],\n  template: `<h1>Count: {{ count() }}</h1>`\n})\nexport class CounterComponent {\n  count = signal(0);\n}\n```";
        }

        // 7. Quizzes & Certification
        if (msg.Contains("quiz") || msg.Contains("certificate") || msg.Contains("cert") || msg.Contains("exam") || msg.Contains("passing score"))
        {
            return "### 🏆 SkillHub Quizzes & Verifiable Certificates\n\n" +
                   "- Every course module features an interactive assessment quiz.\n" +
                   "- A score of **70% or higher** is required to pass and complete the curriculum.\n" +
                   "- Upon completing all lessons and passing the quizzes, your official **SkillHub Certificate of Completion** is automatically generated with a unique verifiable credential ID for your resume and LinkedIn!";
        }

        // 8. Enrollment & Pricing in INR
        if (msg.Contains("enroll") || msg.Contains("how to buy") || msg.Contains("purchase") || msg.Contains("payment") || msg.Contains("price"))
        {
            return "### 🎓 How to Enroll in a SkillHub Course\n\n" +
                   "1. Head to the **[Courses](/courses)** page.\n" +
                   "2. Select any course to view its curriculum, instructor, and discounted price in Indian Rupees (**₹**).\n" +
                   "3. Click **Enroll Now** to proceed through our secure checkout.\n" +
                   "4. Access your video player, module notes, and quizzes immediately under **[My Learning](/my-learning)**!";
        }

        // 9. Becoming an Instructor / Course Creation
        if (msg.Contains("become an instructor") || msg.Contains("create course") || msg.Contains("teach on skillhub") || msg.Contains("publish course"))
        {
            return "### 👨‍🏫 Teach on SkillHub: Course Creation Workflow\n\n" +
                   "1. Sign in with an Instructor or Admin account.\n" +
                   "2. Click **+ Create Course** in the top navigation bar or go to **[Create Course](/courses/create)**.\n" +
                   "3. Enter course title, description, category, and set your regular and discounted price in **₹ (INR)**.\n" +
                   "4. Organize sections, upload video lessons and quiz questions, and publish!";
        }

        // 10. Live Course Catalog / Recommendation
        if (msg.Contains("course") || msg.Contains("recommend") || msg.Contains("learn"))
        {
            var courses = await _context.Courses
                .Where(c => c.IsPublished)
                .OrderByDescending(c => c.CreatedAt)
                .Take(3)
                .ToListAsync();

            if (courses.Count > 0)
            {
                var sb = new StringBuilder("Here are top recommended courses on SkillHub with live Indian Rupee (₹) pricing:\n\n");
                foreach (var c in courses)
                {
                    if (c.DiscountPrice.HasValue && c.DiscountPrice.Value > 0 && c.DiscountPrice.Value < c.Price)
                    {
                        var pct = Math.Round((c.Price - c.DiscountPrice.Value) / c.Price * 100);
                        sb.AppendLine($"- **[{c.Title}](/courses/{c.Id})** — **₹{c.DiscountPrice.Value:N0}** ~~₹{c.Price:N0}~~ ({pct}% OFF)");
                    }
                    else if (c.Price > 0)
                    {
                        sb.AppendLine($"- **[{c.Title}](/courses/{c.Id})** — **₹{c.Price:N0}**");
                    }
                    else
                    {
                        sb.AppendLine($"- **[{c.Title}](/courses/{c.Id})** — **Free**");
                    }
                }
                sb.AppendLine("\nFeel free to ask me questions about any topic or module!");
                return sb.ToString();
            }

            return "SkillHub offers hands-on courses in **.NET Core, Angular, Microservices, and Cloud Architecture**. You can explore our full catalog on the **[Courses](/courses)** page!";
        }

        // 11. Support & Helpdesk
        if (msg.Contains("support") || msg.Contains("ticket") || msg.Contains("helpdesk") || msg.Contains("contact us") || msg.Contains("customer care"))
        {
            return "Have an issue with your lessons, video playback, payment, or account? You can open a support ticket anytime at the **[Help & Support Center](/support)**, and our staff will assist you promptly.";
        }

        return "I am your SkillHub AI Learning Copilot! I can help you discover courses with live INR (₹) pricing, explain programming concepts, solve aptitude problems (Trains, Time & Work, Interest), prepare for technical interviews, and guide you through platform features. What would you like to explore today?";
    }

    private async Task<string> BuildFactualSystemPromptAsync()

    {

        var publishedCourses = await _context.Courses

            .Where(c => c.IsPublished)

            .Include(c => c.Category)

            .Include(c => c.Instructor)

            .OrderByDescending(c => c.CreatedAt)

            .ToListAsync();



        var catalogSb = new StringBuilder();

        if (publishedCourses.Count > 0)

        {

            catalogSb.AppendLine("### OFFICIAL LIVE SKILLHUB COURSE CATALOG (FACTUAL GROUND TRUTH):");

            foreach (var c in publishedCourses)

            {

                var instructor = c.Instructor != null ? $"{c.Instructor.FirstName} {c.Instructor.LastName}".Trim() : "SkillHub Expert Faculty";

                var category = c.Category != null ? c.Category.Name : "Technical Education";

                catalogSb.AppendLine($"- Course ID: {c.Id}");

                catalogSb.AppendLine($"  Title: '{c.Title}'");

                catalogSb.AppendLine($"  Category: {category}");

                catalogSb.AppendLine($"  Instructor: {instructor}");

                if (c.DiscountPrice.HasValue && c.DiscountPrice.Value > 0 && c.DiscountPrice.Value < c.Price)

                {

                    var pct = Math.Round((c.Price - c.DiscountPrice.Value) / c.Price * 100);

                    catalogSb.AppendLine($"  Active Discounted Price: ₹{c.DiscountPrice.Value:N0} (Regular Price: ₹{c.Price:N0}, Savings: {pct}% OFF)");

                }

                else if (c.Price > 0)

                {

                    catalogSb.AppendLine($"  Price: ₹{c.Price:N0}");

                }

                else

                {

                    catalogSb.AppendLine("  Price: Free");

                }

                catalogSb.AppendLine($"  Description: {c.Description}");

                catalogSb.AppendLine($"  Direct Page Link: [{c.Title}](/courses/{c.Id})");

                catalogSb.AppendLine();

            }

        }



        return $@"You are the official SkillHub AI Learning Copilot & Educational Assistant.

SkillHub is a premier online learning platform engineered specifically for the Indian tech education market.



{catalogSb}



### CRITICAL RULES & INSTRUCTIONS (MANDATORY COMPLIANCE):

1. CURRENCY STANDARD:

   - ALL prices, discounts, and fees MUST ALWAYS be quoted in Indian Rupees using the Rupee symbol '₹' (e.g., '₹1,999', '₹2,499').

   - NEVER, under any circumstance, output US dollars ('$'), USD, or other foreign currencies. SkillHub operates in India.

2. FACTUAL PRICING & CATALOG DATA:

   - When asked about courses, tuition, discounts, or recommendations, ALWAYS rely on the factual live catalog provided above.

   - Quote the EXACT active discounted price and highlight the discount percentage (e.g., '₹2,499 (58% OFF from ₹5,999)').

   - Always link to the course using the exact markdown format: [Course Title](/courses/{{id}}).

   - Do NOT invent or hallucinate course names or prices not listed in the factual catalog.

3. SPECIALIZED KNOWLEDGE DOMAINS:

   a) APTITUDE & PLACEMENT INTERVIEW COACHING:

      - When asked quantitative aptitude questions (e.g., Trains & Platforms, Time & Work, Simple/Compound Interest, Profit & Loss, Pipes & Cisterns), provide clear, step-by-step solutions using both the fundamental formula AND the fastest shortcut method (e.g., LCM unit method for work, relative speed conversion for trains).

      - Include practical numerical examples and practice problems.

   b) TECHNICAL INTERVIEW PREPARATION:

      - Offer deep, rigorous explanations of core .NET 8 (DI, Middleware, GC, Async/Await, Task Parallel Library, EF Core performance, Clean Architecture), Angular (Signals, Standalone Components, RxJS, OnPush change detection), and AI/Data Science (RAG architecture, Transformers, Embeddings, PyTorch).

      - Always accompany explanations with clean, production-grade code snippets.

   c) CAREER ROADMAPS & GUIDANCE:

      - Provide practical learning roadmaps tailored to the Indian software engineering market, highlighting high-demand tech stacks and direct SkillHub course recommendations.



4. PLATFORM WORKFLOWS & NAVIGATION:

   - Enrollment & Payment: Explain how students browse [Courses](/courses), see clear discount pricing in Indian Rupees (₹), and enroll seamlessly.

   - Classroom Player: Students learn through video lectures, track lesson completion, and access transcripts at /learn/{{id}}.

   - Quizzes & Certification: Course modules conclude with evaluation quizzes. Scoring 70% or higher awards a verifiable SkillHub Certificate of Completion with a unique credential ID.

   - Support & Helpdesk: Direct users with payment, account, or lecture issues to open a support ticket at [Help & Support Center](/support).

   - Instructor Portal: Instructors and Admins can build and publish courses via [Create Course](/courses/create).



5. RESPONSE PROFUNDITY & PRESENTATION:

   - Never give curt, surface-level answers. Deliver insightful, thorough, and pedagogically sound responses.

   - Structure answers using bold highlights, clean headings, bullet points, and syntax-highlighted code fences.

   - Maintain an encouraging, authoritative academic mentor tone that empowers learners.";

    }



    public List<string> GetQuickSuggestions(string? contextUrl)

    {

        if (!string.IsNullOrWhiteSpace(contextUrl) && contextUrl.Contains("/learn"))

        {

            return new List<string>

            {

                "Explain this lesson in simple terms",

                "Give me a code practice challenge",

                "What are the key takeaways?",

                "How will this be tested in the quiz?"

            };

        }



        if (!string.IsNullOrWhiteSpace(contextUrl) && contextUrl.Contains("/courses"))

        {

            return new List<string>

            {

                "Recommend courses for beginners",

                "What are the most popular courses?",

                "How do course enrollments work?",

                "Are there free courses available?"

            };

        }



        return new List<string>

        {

            "Recommend top courses for me",

            "Explain Dependency Injection in .NET",

            "How do quizzes and certificates work?",

            "How do I open a support ticket?"

        };

    }



    private List<string> GenerateDynamicSuggestions(string userQuery, string? contextUrl)

    {

        var q = userQuery.ToLower();

        if (q.Contains("course") || q.Contains("recommend"))

        {

            return new List<string>

            {

                "Show me beginner-friendly courses",

                "How do I enroll in a course?",

                "What skills will I learn?"

            };

        }



        if (q.Contains("di") || q.Contains("code") || q.Contains("c#") || q.Contains(".net"))

        {

            return new List<string>

            {

                "What is the difference between Scoped & Transient?",

                "Can you give an example with DbContext?",

                "Recommend a course on this topic"

            };

        }



        return GetQuickSuggestions(contextUrl);

    }

}

