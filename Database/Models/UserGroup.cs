namespace ChapubelichBot.Database.Models
{
    public class UserGroup
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public long GroupId { get; set; }
        public Group Group { get; set; }
    }
}
