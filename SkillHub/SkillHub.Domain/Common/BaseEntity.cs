namespace SkillHub.SkillHub.Domain.Common
{
    public abstract class BaseEntity
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
//almost every table will have id , createdat and updatedat so we can create a
//base entity class and inherit it in other entities to avoid code duplication