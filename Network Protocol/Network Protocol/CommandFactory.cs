using System;
using System.Collections.Generic;

namespace Network_Protocol
{
    public abstract class CommandFactory
    {
        private readonly Dictionary<int, Type> m_CommandsToIDDictionary;
        private readonly Dictionary<Type, int> m_IDToCommandsDictionary;
        private int m_Counter;

        protected CommandFactory()
        {
            m_CommandsToIDDictionary = new Dictionary<int,Type>();
            m_IDToCommandsDictionary = new Dictionary<Type, int>(); 
            AddCommand(typeof(CloseCommand));
        }

        protected void AddCommand(Type commandType)
        {
            if (!m_IDToCommandsDictionary.ContainsKey(commandType))
            {
                m_CommandsToIDDictionary.Add(m_Counter, commandType);
                m_IDToCommandsDictionary.Add(commandType, m_Counter);
                m_Counter++;
            }
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
            var commandType = command.GetType();
            if (m_IDToCommandsDictionary.ContainsKey(commandType))
            {
                return m_IDToCommandsDictionary[commandType];
            }
            throw new ArgumentException("Commmand not found in factory");
        }
    }


    public class TestCommandFactory : CommandFactory
    {
        public TestCommandFactory()
        {            
            AddCommand(typeof(SomeCommand));
            AddCommand(typeof(CloseCommand));
            AddCommand(typeof(HelloWorldCommand));
        }
    }
}
