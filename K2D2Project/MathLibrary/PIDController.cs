namespace KSP2FlightAssistant.MathLibrary
{
    public class PIDController
    {
        public float kP { get; set; }
        public float kI { get; set; }
        public float kD { get; set; }
        
        private float previousError { get; set; }
        
        private float lastUpdate { get; set; }
        
        private float I_before { get; set; }


        PIDController(float kP, float kI, float kD)
        {
            this.kP = kP;
            this.kI = kI;
            this.kD = kD;
            lastUpdate = float.PositiveInfinity;
            previousError = float.PositiveInfinity;
            I_before = float.PositiveInfinity;
        }

        /// <summary>
        /// Calculates the proportional of the error
        /// </summary>
        /// <param name="error"></param>
        /// <returns>
        /// Calculated proportional
        /// </returns>
        public float CalculateProportional(float error)
        {
            return kP * error;
        }
        
        
        /// <summary>
        /// Calculates the integral of the error
        /// </summary>
        /// <param name="error"></param>
        /// <param name="dt"></param>
        /// <returns>
        /// float value of the integral
        /// </returns>
        public float CalculateIntegral(float error, float dt)
        {
            if (float.IsPositiveInfinity(I_before))
            {
                I_before = 0;
            }
            
            float integral = I_before + (error * dt);
            I_before = integral;
            return kI * integral;
        }
            

        /// <summary>
        /// Calculates the derivative of the error
        /// </summary>
        /// <param name="error"></param>
        /// <param name="dt"></param>
        /// <returns>
        /// float value of the derivative
        /// </returns>
        public float CalculateDerivative(float error, float dt)
        {
            float derivative = (error - previousError) / dt;
            previousError = error;
            return kD * derivative;
        }
        
        
        /// <summary>
        /// Calculates the delta time between the last update and the current update
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public float delta_time(float currentTime)
        {

            float dt = currentTime - lastUpdate;
            lastUpdate = currentTime;
            return dt;
        }

        public float Output(float error,float currentTime)
        {
            if (float.IsPositiveInfinity(lastUpdate))
            {
                lastUpdate = currentTime;
            }
            
            if (float.IsPositiveInfinity(previousError))
            {
                previousError = error;
            }
            
            float dt = delta_time(currentTime);
            return CalculateProportional(error) + CalculateIntegral(error, dt) + CalculateDerivative(error, dt);
        }
    }
}