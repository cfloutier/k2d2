using System;

namespace KSP2FlightAssistant.MathLibrary
{
    public static class VisVivaEquation
    {
        static double GravityConstant = 6.67408e-11;
        
        
        /// <summary>
        /// Calculates the velocity of a body in orbit
        /// </summary>
        /// <param name="CurrentDistance"></param>
        /// <param name="Apoapsis"></param>
        /// <param name="Periapsis"></param>
        /// <param name="PlanetaryMass"></param>
        /// <returns></returns>
        public static double CalculateVelocity(double CurrentDistance, double Apoapsis, double Periapsis,
            double gravitation, double eccentricity)
        {
            if (eccentricity >= 1 || Double.IsInfinity(Apoapsis))
                return Math.Sqrt(gravitation*(2/Periapsis-1/(Periapsis/(eccentricity-1))));

            if(Apoapsis-Periapsis<100)
                return Math.Sqrt(gravitation / CurrentDistance);

            double semiMajorAxis = (Apoapsis + Periapsis) / 2;
            return Math.Sqrt(gravitation * ((2 / CurrentDistance) - (1 / semiMajorAxis)));
        }

        public static double CalculateHyperbolicVelocity(double distance, double gravitation, double orbitalEnergy)
        {

            double a = -gravitation / (2 * orbitalEnergy);
            return Math.Sqrt(gravitation * (2 / distance - 1 / a));

        }
        

        public static double CalculateGravitation(double CurrentDistance, double Apoapsis, double Periapsis,
            double Velocity)
        {
            double MajorSemiAxis = (Apoapsis + Periapsis) / 2;
            return (Velocity*Velocity)/(2/CurrentDistance-1/MajorSemiAxis);
        }
        



    }
}