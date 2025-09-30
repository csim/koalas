using System;

namespace Koalas.Text.Models;

public record class TextLineModel(string Text) : IRender
{
    public string Render()
        => Text + Environment.NewLine;
}