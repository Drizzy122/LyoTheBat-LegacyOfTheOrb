using System.Threading.Tasks;
using UnityEngine;

public interface ICommand
{
    Task Execute();
}
