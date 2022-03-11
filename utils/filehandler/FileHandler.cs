namespace dc.assignment.primenumbers.utils.filehandler{
    public class NumbersFileHandler{
        private string[] fileLines;
        private string completedFile;
        private int currentNumber = 0;
        private int currentNumberPosition = -1;
        private bool currentNumberCompleted = true;

        public NumbersFileHandler(string inputFile, string completedFile){
            this.fileLines = System.IO.File.ReadAllLines(inputFile);
            this.completedFile = completedFile;
        }

        public int getNextNumber(){
            this.currentNumberCompleted = false;
            return 0;
        }

        public void markNumberAsComplete(){
            this.currentNumber = 0;
            this.currentNumberCompleted = true;
        }
    }
}