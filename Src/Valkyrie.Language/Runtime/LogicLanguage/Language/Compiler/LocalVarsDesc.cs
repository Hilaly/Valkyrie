using System;
using System.Text;
using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Compiler
{
    class LocalVarsDesc : StringToIntConverter
    {
        private Variable[] _buffer;

        public Variable[] GetBuffer()
        {
            if (_buffer == null)
                _buffer = new Variable[Count];
            Array.Clear(_buffer, 0, _buffer.Length);
            /*for (var i = 0; i < _buffer.Length; ++i)
                _buffer[i] = Variable.Null;*/
            return _buffer;
        }

        public void CopyTo(Variable[] outputBuffer)
        {
            Array.Copy(_buffer, outputBuffer, _buffer.Length);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[ ");
            for (int i = 0; i < Count; i++) 
                sb.AppendFormat("{0}->{1} ", GetString(i), _buffer[i]);
            sb.Append("]");
            return sb.ToString();
        }
    }
}