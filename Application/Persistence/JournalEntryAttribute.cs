﻿using System;

namespace Application.Persistence
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class JournalEntryAttribute : Attribute
    {
        public string Manifest { get; }

        public JournalEntryAttribute(string manifest)
        {
            Manifest = manifest;
        }
    }
}
