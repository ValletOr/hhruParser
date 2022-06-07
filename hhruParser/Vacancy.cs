namespace hhruParser
{
    internal class Vacancy
    {
        public string name { get; set; }
        public int wage { get; set; }

        public Vacancy(string n, int w)
        {
            this.name = n;
            this.wage = w;
        }

        public string getName()
        {
            return name;
        }
        public int getWage()
        {
            return wage;
        }

        public void setName(string n)
        {
            name = n;
        }

        public void setWage(int w)
        {
            wage = w;
        }
    }
}
