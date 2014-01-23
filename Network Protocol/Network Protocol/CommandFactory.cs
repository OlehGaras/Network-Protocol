﻿using System;
using System.Collections.Generic;

namespace Network_Protocol
{
    public abstract class CommandFactory
    {
        private readonly Dictionary<int, Type> m_CommandsToIDDictionary;
        private readonly Dictionary<Type, int> m_IDToCommandsDictionary;
        private int m_Counter = 0;

        protected CommandFactory()
        {
            m_CommandsToIDDictionary = new Dictionary<int,Type>();
            AddCommand(typeof(CloseCommand));
        }

        protected void AddCommand(Type commandType)
        {
            m_CommandsToIDDictionary.Add(m_Counter, commandType);
            m_IDToCommandsDictionary.Add(commandType, m_Counter);
            m_Counter++;
        }

        public Command GetCommandByID(int id)
        {
            if (m_CommandsToIDDictionary.ContainsKey(id))
            {
                return (Command)Activator.CreateInstance(m_CommandsToIDDictionary[id]);                
            }
            return null;
        }

        public int GetCommandID(Command command)
        {
            Type commandType = command.GetType();
            if (m_IDToCommandsDictionary.ContainsKey(commandType))
            {
                return m_IDToCommandsDictionary[commandType];
            }
            throw new ArgumentException("Commmand not found in factory");
        }
    }


    public class DisplayCommandFactory : CommandFactory
    {
        public DisplayCommandFactory()
        {
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
            AddCommand(typeof(Type));
        }
    }
}