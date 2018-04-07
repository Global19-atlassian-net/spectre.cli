﻿using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.CommandLine.Internal.Rendering.Elements;

namespace Spectre.CommandLine.Internal.Rendering
{
    internal sealed class RenderableComposer : IRenderable
    {
        private BlockElement Root { get; }

        public RenderableComposer()
        {
            Root = new BlockElement();
        }

        public RenderableComposer LineBreak()
        {
            Root.Append(new LineBreakElement());
            return this;
        }

        public RenderableComposer Text(string text)
        {
            if (text != null)
            {
                Root.Append(new TextElement(text));
            }
            return this;
        }

        public RenderableComposer Condition(bool condition,
            Action<RenderableComposer> @true,
            Action<RenderableComposer> @false)
        {
            if (condition)
            {
                var block = new RenderableComposer();
                @true(block);
                Root.Append(block.Root);
            }
            else
            {
                var block = new RenderableComposer();
                @false(block);
                Root.Append(block.Root);
            }

            return this;
        }

        public RenderableComposer Raw(IRenderable renderable)
        {
            Root.Append(renderable);
            return this;
        }

        public RenderableComposer Join(string separator, IEnumerable<IRenderable> items)
        {
            var array = items.ToArray();
            for (var i = 0; i < array.Length; i++)
            {
                Root.Append(array[i]);
                if (i != array.Length - 1)
                {
                    Root.Append(new TextElement(separator));
                }
            }
            return this;
        }

        public RenderableComposer Tab()
        {
            return Tabs(1);
        }

        public RenderableComposer Tabs(int count)
        {
            Spaces(count * 4);
            return this;
        }

        public RenderableComposer Space()
        {
            return Spaces(1);
        }

        public RenderableComposer Spaces(int count)
        {
            Repeat(' ', count);
            return this;
        }

        public RenderableComposer Repeat(char character, int count)
        {
            Root.Append(new RepeatingElement(count, new TextElement(character.ToString())));
            return this;
        }

        public RenderableComposer Block(Action<RenderableComposer> action)
        {
            var block = new RenderableComposer();
            action(block);
            Root.Append(block.Root);
            return this;
        }

        public RenderableComposer Color(ConsoleColor color, Action<RenderableComposer> action)
        {
            var content = new RenderableComposer();
            action(content);
            Root.Append(new ColorElement(color, content.Root));
            return this;
        }

        public RenderableComposer Empty()
        {
            return this;
        }

        public void Render(IRenderer renderer)
        {
            Root.Render(renderer);
        }
    }
}