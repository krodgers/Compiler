using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    class StackList : List<BasicBlock>
    {
        public void Push(BasicBlock block)
        {
            this.Add(block);
        }

        public BasicBlock Pop()
        {
            BasicBlock returnBlock = this.Last();
            this.RemoveAt(this.Count - 1);
            return returnBlock;
        }

        public BasicBlock Peek()
        {
            return this.Last();
        }

        public BasicBlock GetOuterJoin()
        {
            if (this.Count > 1)
            {
                var element = this.ElementAt(this.Count - 2);
                return element;
            }
            else
            {
                return null;
            }
        }
    }
}
