using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
	    
	public interface IHeapItem<in T> : IComparable<T> 
	{
		int HeapIndex {get;set;}
	}
	
    public class Heap<T>
    where T : IHeapItem<T>
    {
	    private readonly T[] items;
	    private int currentItemCount;
	    
	    public Heap(int maxHeapSize)
	    {
	        items = new T[maxHeapSize];
	    }

	    public void Add(T item)
	    {
	        item.HeapIndex = currentItemCount;
			items[currentItemCount] = item;
			
			SortUp(item);
			
			currentItemCount++;
	    }
	    public T RemoveFirst() 
	    {
			T firstItem = items[0];
			currentItemCount--;
			
			items[0] = items[currentItemCount];
			items[0].HeapIndex = 0;
			
			SortDown(items[0]);
			return firstItem;
		}

		public void UpdateItem(T item) => SortUp(item);

		public int Count => currentItemCount;

		public bool Contains(T item) => Equals(items[item.HeapIndex], item);

		private void SortDown(T item) 
	    {
			while (true) 
	        {
				int childIndexLeft = item.HeapIndex * 2 + 1;
				int childIndexRight = item.HeapIndex * 2 + 2;
				int swapIndex = 0;

				if (childIndexLeft < currentItemCount) //check if left child exist(if childindex > item count => parent was at the bottom of the tree)
	            {
					swapIndex = childIndexLeft;

					if (childIndexRight < currentItemCount) //check if right child exist
	                {
						if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0) //check if leftIndex precedes rightIndex
	                    {
							swapIndex = childIndexRight;
						}
					}

					if (item.CompareTo(items[swapIndex]) < 0) 
	                {
						Swap (item,items[swapIndex]);
					}
					else 
	                {
						return;
					}

				}
				else 
	            {
					return;
				}

			}
		}

		private void SortUp(T item) 
	    {
			int parentIndex = (item.HeapIndex-1)/2;
			
			while (true) 
	        {
				T parentItem = items[parentIndex];
				if (item.CompareTo(parentItem) > 0) 
	            {
					Swap (item,parentItem);
				}
				else 
	            {
					break;
				}

				parentIndex = (item.HeapIndex-1)/2;
			}
		}
		
		private void Swap(T itemA, T itemB) 
	    {
			items[itemA.HeapIndex] = itemB;
			items[itemB.HeapIndex] = itemA;
			(itemA.HeapIndex, itemB.HeapIndex) = (itemB.HeapIndex, itemA.HeapIndex);
	    }
    }

}
