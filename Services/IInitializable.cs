namespace _01S.Services
{
    /// <summary>
    /// Интерфейс для ViewModel, принимающих параметр при навигации.
    /// </summary>
    /// <typeparam T="T">Тип передаваемого параметра (модель данных).</typeparam>
    interface IInitializable<T>
    {
        void Initialize(T? parameter);
    }
}
