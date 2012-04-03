using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace TimeTable
{
   public class ViewModelBase : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;

      //Traditional OnPropertyChanded method
      public void OnPropertyChanged(string propertyName)
      {
         PropertyChangedEventHandler handler = PropertyChanged;

         if (handler != null)
         {
            handler(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      //New and improved OnPropertyChanged method
      protected void OnPropertyChanged<T>(Expression<Func<T>> expression)
      {
         OnPropertyChanged(GetProperty(expression).Name);
      }

      //Helper method
      public static PropertyInfo GetProperty<T>(Expression<Func<T>> expression)
      {
         var property = GetMember(expression) as PropertyInfo;
         if (property == null)
         {
            throw new ArgumentException("Not a property expression", GetMember(() => expression).Name);
         }

         return property;
      }

      //Helper method
      public static MemberInfo GetMember<T>(Expression<Func<T>> expression)
      {
         if (expression == null)
         {
            throw new ArgumentNullException(GetMember(() => expression).Name);
         }

         return GetMemberInfo(expression);
      }

      //Helper method
      public static MemberInfo GetMemberInfo(LambdaExpression lambda)
      {
         if (lambda == null)
         {
            throw new ArgumentNullException(GetMember(() => lambda).Name);
         }

         MemberExpression memberExpression = null;
         if (lambda.Body.NodeType == ExpressionType.Convert)
         {
            memberExpression = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
         }
         else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
         {
            memberExpression = lambda.Body as MemberExpression;
         }
         else if (lambda.Body.NodeType == ExpressionType.Call)
         {
            return ((MethodCallExpression)lambda.Body).Method;
         }

         if (memberExpression == null)
         {
            throw new ArgumentException("Not a member access", GetMember(() => lambda).Name);
         }

         return memberExpression.Member;
      } 
   }
}
