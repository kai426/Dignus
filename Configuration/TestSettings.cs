namespace Dignus.Candidate.Back.Configuration
{
    /// <summary>
    /// Configuration settings for test generation and management
    /// </summary>
    public class TestSettings
    {
        /// <summary>
        /// Configuration section key
        /// </summary>
        public const string SectionName = "TestSettings";

        /// <summary>
        /// Number of questions per test type
        /// </summary>
        public QuestionCountSettings QuestionCounts { get; set; } = new();

        /// <summary>
        /// Test timeout in minutes per test type
        /// </summary>
        public TestTimeoutSettings TestTimeouts { get; set; } = new();
    }

    /// <summary>
    /// Configuration for number of questions per test type
    /// </summary>
    public class QuestionCountSettings
    {
        /// <summary>
        /// Number of Portuguese test questions
        /// </summary>
        public int Portuguese { get; set; } = 10;

        /// <summary>
        /// Number of Math test questions
        /// </summary>
        public int Math { get; set; } = 15;

        /// <summary>
        /// Number of Psychology test questions
        /// </summary>
        public int Psychology { get; set; } = 52;

        /// <summary>
        /// Number of Visual Retention test questions
        /// </summary>
        public int VisualRetention { get; set; } = 15;
    }

    /// <summary>
    /// Configuration for test timeout in minutes per test type
    /// </summary>
    public class TestTimeoutSettings
    {
        /// <summary>
        /// Portuguese test timeout in minutes
        /// </summary>
        public int Portuguese { get; set; } = 30;

        /// <summary>
        /// Math test timeout in minutes
        /// </summary>
        public int Math { get; set; } = 45;

        /// <summary>
        /// Psychology test timeout in minutes
        /// </summary>
        public int Psychology { get; set; } = 25;

        /// <summary>
        /// Visual Retention test timeout in minutes
        /// </summary>
        public int VisualRetention { get; set; } = 20;
    }
}