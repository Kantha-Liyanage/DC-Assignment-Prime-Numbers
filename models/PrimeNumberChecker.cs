namespace dc.assignment.primenumbers.models{

    class PrimeNumberChecker{

        private int theNumber;
        private int fromNumber;
        private int toNumber;
        private bool _isChecking;
        private bool _abort;
        public event EventHandler? onPrimeNumberDetected;
        public event EventHandler<PrimeNumberNotDetectedEventArgs>? onPrimeNumberNotDetected;
        public PrimeNumberChecker(int theNumber, int fromNumber, int toNumber){
            this.theNumber = theNumber;
            this.fromNumber = fromNumber;
            this.toNumber = toNumber;
            this._isChecking = false;
        }

        public bool isChecking(){
            return _isChecking;
        }

        public void abort(){
            this._abort = true;
        }

        public bool start(){
            if(!isValidInput()){
                return false;
            }

            this._isChecking = true;
            this._abort = false;

            var thread = new Thread(() => { 
                // work of the work
                int currentNumber = this.fromNumber;
                bool isPrimeNumber = true;

                while(currentNumber <= this.toNumber){
                    // abort check
                    if(this._abort){
                        break;
                    }
                    
                    Thread.Sleep(1);
                    if(this.theNumber % currentNumber == 0){
                        if(this.theNumber == currentNumber){
                            isPrimeNumber = true;
                            break;
                        }
                        else{
                            isPrimeNumber = false;
                            break;
                        }
                    }
                    currentNumber++;
                }

                // inform 
                if(!this._abort){
                    if(isPrimeNumber){
                        onPrimeNumberDetected?.Invoke(this, EventArgs.Empty);
                    }
                    else{
                        // can be devide by "currentNumber"
                        onPrimeNumberNotDetected?.Invoke(this, new PrimeNumberNotDetectedEventArgs(currentNumber));
                    }
                }
                this._isChecking = false;
            });
            thread.Start();

            return true;
        }

        private bool isValidInput(){
            if(theNumber <=2 || fromNumber <= 1 ||toNumber <= 1){
                return false;
            }

            if(toNumber < fromNumber){
                return false;
            }

            return true;
        }
    }

}