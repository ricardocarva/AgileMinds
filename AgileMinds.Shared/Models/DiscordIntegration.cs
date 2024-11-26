using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AgileMinds.Shared.Models
{
    [Table("DiscordIntegrations")]  // Explicitly define the table name
    public class DiscordIntegration
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("Project")]
        [Column("project_id")]
        public int ProjectId { get; set; }

        [Column("discord_server_id")]
        public string DiscordServerId { get; set; }

        [Column("discord_bot_token")]
        public string DiscordBotToken { get; set; }

        [Column("discord_channel_id")]
        public string DiscordChannelId { get; set; }

        [Column("discord_project_secret")]
        public string DiscordProjectSecret { get; set; }

        [Column("is_linked")]
        public bool IsLinked { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation property to the Project
        [JsonIgnore]
        public virtual Project? Project { get; set; }
    }
}
