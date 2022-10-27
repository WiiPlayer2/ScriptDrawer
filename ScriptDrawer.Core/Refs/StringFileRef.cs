using System;
using ScriptDrawer.Core.Refs.Mappers;

namespace ScriptDrawer.Core.Refs;

[Obsolete]
public class StringFileRef : FileRef<string, StreamToStringMapper>
{
    public StringFileRef(string filePath) : base(filePath) { }
}
