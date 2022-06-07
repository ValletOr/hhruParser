namespace hhruParser
{
    internal class Job
    {
        private int id;
        private string name;
        
        public Job(string inpStr)
        {
            string[] splitStr = inpStr.Split(';');
            id = int.Parse(splitStr[0]);
            name = splitStr[1];
        }

        public int getId() { return id; }
        public string getName() { return name; }
        public void makeMark()
        {
            name = "V " + name;
        }
        public void delMark()
        {
            name = name.Substring(2);
        }

        public override string ToString()
        {
            return this.name;
        }
    }
}
