namespace dc.assignment.primenumbers.utils.filehandler{
    public class NumbersFileHandler{
        private string[] fileLines;
        private string statusFile;
        private int currentNumber = 0;
        private int currentNumberPosition = -1;
        public NumbersFileHandler(string inputFile, string statusFile){
            this.fileLines = System.IO.File.ReadAllLines(inputFile);
            this.statusFile = statusFile;
            this.currentNumberPosition = int.Parse(System.IO.File.ReadAllText(statusFile));
        }

        public int getNextNumber(){
            currentNumberPosition++;
            currentNumber = int.Parse(fileLines[currentNumberPosition]);
            return currentNumber;
        }

        public void markNumberAsComplete(){
            System.IO.File.WriteAllText(statusFile, this.currentNumberPosition.ToString());
        }
    }
}