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
            double PlanetaryMass)
        {


            double gravitation = PlanetaryMass * GravityConstant;

            if (Double.IsInfinity(Apoapsis))
            {
                // Parabolic orbit (apoapsis is infinity)
                return Math.Sqrt(gravitation * (2 / CurrentDistance));
            }

            double semiMajorAxis = (Apoapsis + Periapsis) / 2;
            return Math.Sqrt(gravitation * ((2 / CurrentDistance) - (1 / semiMajorAxis)));
        }




    }
}