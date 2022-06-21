using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Valkyrie.Language.Ecs
{
    public struct Fact
    {
        public static Fact Empty = new Fact { Id = -1 };
        
        public int Id;
        
        public Variable Arg0;
        public Variable Arg1;
        public Variable Arg2;
        public Variable Arg3;
        
        public byte ArgsCount;

        #region Ctors

        public Fact(int id, params Variable[] args) : this(id, (IEnumerable<Variable>)args)
        {
        }
        
        public Fact(int id, IEnumerable<Variable> args) : this()
        {
            Id = id;
            foreach (var variable in args)
            {
                this[ArgsCount] = variable;
                ArgsCount++;
            }
        }

        #endregion

        public string ToString(IWorld world)
        {
            var name = world.GetFactName(Id);
            switch (ArgsCount)
            {
                case 4:
                    return $"[{name} {Arg0} {Arg1} {Arg2} {Arg3}]";
                case 3:
                    return $"[{name} {Arg0} {Arg1} {Arg2}]";
                case 2:
                    return $"[{name} {Arg0} {Arg1}]";
                case 1:
                    return $"[{name} {Arg0}]";
                case 0:
                    return $"[{name}]";
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public Variable this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Arg0;
                    case 1: return Arg1;
                    case 2: return Arg2;
                    case 3: return Arg3;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Arg0 = value;
                        break;
                    case 1:
                        Arg1 = value;
                        break;
                    case 2:
                        Arg2 = value;
                        break;
                    case 3:
                        Arg3 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public Variable[] GetArgs()
        {
            var r = new Variable[ArgsCount];
            for (var i = 0; i < ArgsCount; ++i)
                r[i] = this[i];
            return r;
        }

        #region Compare

        public bool Equals(Fact other)
        {
            if (Id != other.Id || ArgsCount != other.ArgsCount)
                return false;
            for (var i = 0; i < ArgsCount; ++i)
            {
                if (!this[i].Equals(other[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is Fact other && Equals(other);
        }

        public override int GetHashCode()
        {
            switch (ArgsCount)
            {
                case 4:
                    return HashCode.Combine(Id, Arg0, Arg1, Arg2, Arg3, ArgsCount);
                case 3:
                    return HashCode.Combine(Id, Arg0, Arg1, Arg2, ArgsCount);
                case 2:
                    return HashCode.Combine(Id, Arg0, Arg1, ArgsCount);
                case 1:
                    return HashCode.Combine(Id, Arg0, ArgsCount);
                case 0:
                    return HashCode.Combine(Id, ArgsCount);
                default:
                    return HashCode.Combine(Id, Arg0, Arg1, Arg2, ArgsCount);
            }
        }

        public static bool operator ==(Fact left, Fact right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Fact left, Fact right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}