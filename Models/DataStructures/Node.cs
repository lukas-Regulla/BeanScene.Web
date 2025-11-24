namespace BeanScene.Web.Models.DataStructures
{
    public class Node
    {
        public int Data;
        public Node Next;
        public Node Prev;

        public Node(int data)
        {
            Data = data;
            Prev = null;
            Next = null;
        }
    }
}
