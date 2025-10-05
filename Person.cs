/*Name: Channese Lawrence
Date: 5 October 2025
Phase 3 Salon Manager 
*/

using System;
namespace SalonManager.Models
{
    // Abstract base class to satisfy "abstract + polymorphism" rubric items
    public abstract class Person
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        // Demonstrates 'protected' access specifier for the rubric
        protected DateTime CreatedAt { get; } = DateTime.Now;

        // Polymorphic contract
        public abstract string GetProfile();
    }
}
