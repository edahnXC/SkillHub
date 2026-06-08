namespace SkillHub.SkillHub.Domain.Common
{
    public abstract class AuditableEntity:BaseEntity
    {
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
    }
}
//this class inherits from baseentity and adds two properties 
//createdby and updatedby to keep track of who created and updated the record