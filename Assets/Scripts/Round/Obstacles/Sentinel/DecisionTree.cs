using System.Collections.Generic;

namespace Round.Obstacles.Sentinel
{
    public interface IDTNode
    {
        DTAction Walk();
    }

    public delegate object DTCall(IDTNode node); //permette di chiamare un metodo con condizioni ottenute a runtime

    public class DTDecision : IDTNode
    {
        //metodo chiamato per prendere una decisione
        private DTCall Selector;

        //Dizionario che controlla il valore di ritorno della decisione e segue il collegamento corrispondente
        private Dictionary<object, IDTNode> links;

        //Il nodo viene creato con un selettore e un link vuoto
        public DTDecision(DTCall selector)
        {
            Selector = selector;
            links = new Dictionary<object, IDTNode>();
        }

        //Funzione che aggiunge un link a un nodo
        public void AddLink(object value, IDTNode next)
        {
            links.Add(value, next);
        }

        //Funzione che chiama il selector e controlla se c'� un link valido,
        //in quel caso segue (Walk()) il link
        public DTAction Walk()
        {
            object o = Selector(null);
            return links.ContainsKey(o) ? links[o].Walk() : null;
        }
    }

    public class DTAction : IDTNode
    {
        //Metodo per compiere l'azione
        public DTCall Action;

        public DTAction(DTCall callee)
        {
            Action = callee;
        }

        public DTAction Walk()
        {
            return this;
        }
    }

    public class DecisionTree
    {
        private IDTNode root;

        public DecisionTree(IDTNode start)
        {
            root = start;
        }

        public object walk()
        {
            DTAction result = root.Walk();
            if (result != null)
            {
                return result.Action(null);
            }

            return null;
        }
    }
}