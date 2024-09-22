using System.Xml.Linq;

namespace AgileMindsUI.Client.Pages
{
    public class ProjectItem
    {
        public int ID { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public String Person { get; set; }
        public String Date {get; set;}
        public String Status { get; set; }


        public ProjectItem(int id, String title, String description, string person, string date, string status)
        {
            ID = id;
            Title = title;
            Description = description;
            Person = person;
            Date = date;
            Status = status;

        }


    }


}
