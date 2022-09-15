using System;
namespace DTO_PremierDucts.Entities
{
    public class FileInfo
    {
        public int pathId { get; set; }
        public string pathValue { get; set; }
        public string quote { get; set; }
        public string jobReference { get; set; }
        public FileInfo()
        {
        }
    }
}
