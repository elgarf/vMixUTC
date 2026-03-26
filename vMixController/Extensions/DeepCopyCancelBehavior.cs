using Microsoft.Xaml.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace vMixController.Extensions
{
    /// <summary>
    /// Поведение для отмены изменений в коллекции с использованием глубокого копирования.
    /// Сохраняет полную копию при присоединении 
    /// и восстанавливает ее при установке свойства IsCanceled в true.
    /// </summary>
    public class DeepCopyCancelBehavior : Behavior<FrameworkElement>, IDisposable
    {
        // Храним резервную копию
        private List<object> _backup;
        private bool _isDisposed;

        // --- DependencyProperty для привязки к коллекции ---
        public static readonly DependencyProperty TargetCollectionProperty =
            DependencyProperty.Register(
                nameof(TargetCollection),
                typeof(ICollection),
                typeof(DeepCopyCancelBehavior),
                new PropertyMetadata(null, OnTargetCollectionChanged));

        public ICollection TargetCollection
        {
            get => (ICollection)GetValue(TargetCollectionProperty);
            set => SetValue(TargetCollectionProperty, value);
        }

        // --- DependencyProperty для запуска отмены ---
        public static readonly DependencyProperty IsCancelledProperty =
            DependencyProperty.Register(
                nameof(IsCancelled),
                typeof(bool),
                typeof(DeepCopyCancelBehavior),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsCanceledChanged));

        /// <summary>
        /// Установите в true для запуска отмены. Поведение автоматически сбросит его в false после выполнения.
        /// </summary>
        public bool IsCancelled
        {
            get => (bool)GetValue(IsCancelledProperty);
            set => SetValue(IsCancelledProperty, value);
        }

        // --- Логика поведения ---

        /// <summary>
        /// Вызывается при присоединении Behavior к элементу.
        /// Идеальное место для инициализации ресурсов.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            // Создаем бэкап, если коллекция уже установлена
            MakeBackup();
        }

        /// <summary>
        /// Вызывается при отсоединении Behavior от элемента (например, при закрытии окна).
        /// Это КЛЮЧЕВОЕ место для освобождения ресурсов и предотвращения утечек памяти.
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            // Очищаем ресурсы
            Dispose();
        }

        private static void OnTargetCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DeepCopyCancelBehavior behavior)
            {
                // Просто создаем новый бэкап при смене коллекции.
                // Не нужно вызывать GC.Collect().
                behavior.MakeBackup();
            }
        }

        private static void OnIsCanceledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DeepCopyCancelBehavior behavior && (bool)e.NewValue)
            {
                behavior.ExecuteCancel();
                // Сбрасываем флаг после выполнения, чтобы можно было вызвать отмену снова.
                // Убрана некорректная логика в 'else', которая уничтожала бэкап.
                behavior.IsCancelled = false;
            }
        }

        /// <summary>
        /// Создает глубокую копию коллекции
        /// </summary>
        private void MakeBackup()
        {
            if (TargetCollection == null)
            {
                _backup = null;
                return;
            }

            // Убедимся, что все элементы можно клонировать
            if (TargetCollection.Cast<object>().Any(item => !(item is ICloneable)))
            {
                // Можно бросить исключение или просто проигнорировать, зависит от требований
                // throw new InvalidOperationException("All items in the target collection must implement ICloneable.");
                _backup = null;
                return;
            }

            _backup = new List<object>();
            foreach (ICloneable item in TargetCollection)
            {
                _backup.Add(item.Clone());
            }
        }

        /// <summary>
        /// Восстанавливает коллекцию из сохраненной копии.
        /// </summary>
        private void ExecuteCancel()
        {
            // Не выполняем отмену, если бэкапа нет или коллекция не является списком
            if (_backup == null || !(TargetCollection is IList list)) return;

            // Обновляем целевую коллекцию
            list.Clear();
            foreach (var item in _backup)
            {
                // Создаем еще одну копию, чтобы бэкап остался неизменным для повторных отмен
                if (item is ICloneable cloneableItem)
                    list.Add(cloneableItem.Clone());
                else
                    list.Add(item); // На всякий случай, хотя MakeBackup должен это отсечь
            }
        }

        /// <summary>
        /// Реализация IDisposable для корректной очистки.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            _backup?.Clear();
            _backup = null;

            _isDisposed = true;
            GC.SuppressFinalize(this); // Не вызывать финализатор, т.к. мы уже все очистили
        }

        // Можно добавить финализатор на случай, если Dispose не будет вызван (хотя OnDetaching должен сработать)
        ~DeepCopyCancelBehavior()
        {
            Dispose();
        }
    }
}

