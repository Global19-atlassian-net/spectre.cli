namespace Spectre.Cli.Internal
{
    /// <summary>
    /// Represents something that can be rendered to a console.
    /// </summary>
    internal interface IRenderable
    {
        /// <summary>
        /// Gets the length of the element.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Renders the element using the specified renderer.
        /// </summary>
        /// <param name="renderer">The renderer to use.</param>
        void Render(IRenderer renderer);
    }
}