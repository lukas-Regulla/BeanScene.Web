using System.Text;
using Microsoft.AspNetCore.Mvc;
using BeanScene.Web.Models;
using BeanScene.Web.Models.DataStructures;


namespace BeanScene.Web.Models.DataStructures
{
    public class DoublyLinkedList
    {
        private Node head;

        public void AddLast(int data)
        {
            var newNode = new Node(data);

            if (head == null)
            {
                head = newNode;
                return;
            }

            var current = head;
            while (current.Next != null)
                current = current.Next;

            current.Next = newNode;
            newNode.Prev = current;
        }

        public void AddFirst(int data)
        {
            var newNode = new Node(data);

            if (head == null)
            {
                head = newNode;
                return;
            }

            newNode.Next = head;
            head.Prev = newNode;
            head = newNode;
        }

        public void Delete(int data)
        {
            var current = head;

            while (current != null)
            {
                if (current.Data == data)
                {
                    if (current.Prev != null)
                        current.Prev.Next = current.Next;
                    else
                        head = current.Next;

                    if (current.Next != null)
                        current.Next.Prev = current.Prev;

                    return;
                }

                current = current.Next;
            }
        }

        public string DisplayForward()
        {
            var sb = new StringBuilder();
            var current = head;

            sb.Append("Forward: ");

            while (current != null)
            {
                sb.Append(current.Data + " ");
                current = current.Next;
            }

            return sb.ToString();
        }

        public string DisplayBackward()
        {
            var sb = new StringBuilder();

            if (head == null) return "Backward: (empty)";

            var current = head;
            while (current.Next != null)
                current = current.Next;

            sb.Append("Backward: ");

            while (current != null)
            {
                sb.Append(current.Data + " ");
                current = current.Prev;
            }

            return sb.ToString();
        }

        public void Clear()
        {
            head = null;
        }

        public void AddMany(IEnumerable<int> values)
        {
            foreach (var v in values)
                AddLast(v);
        }

        public bool IsEmpty()
        {
            return head == null;
        }
    }
}
