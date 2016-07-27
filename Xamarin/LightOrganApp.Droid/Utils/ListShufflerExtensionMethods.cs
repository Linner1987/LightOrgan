using System;
using System.Collections.Generic;

namespace LightOrganApp.Droid.Utils
{
    public static class ListShufflerExtensionMethods
    {        
        private static Random _rnd = new Random();
       
        public static void Shuffle<T>(this List<T> listToShuffle, int numberOfTimesToShuffle = 5)
        {           
            List<T> newList = new List<T>();
            
            for (int i = 0; i < numberOfTimesToShuffle; i++)
            {                
                while (listToShuffle.Count > 0)
                {                   
                    int index = _rnd.Next(listToShuffle.Count);
                    
                    newList.Add(listToShuffle[index]);
                    
                    listToShuffle.RemoveAt(index);
                }
                
                listToShuffle.AddRange(newList);
                
                newList.Clear();
            }
        }
    }
}