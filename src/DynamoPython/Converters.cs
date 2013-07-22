﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo;
using Dynamo.FSchemeInterop;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace DynamoPython
{
    internal static class Converters
    {
        internal static FScheme.Value convertPyFunction(Func<IList<dynamic>, dynamic> pyf)
        {
            return FScheme.Value.NewFunction(
                FSharpFunc<FSharpList<FScheme.Value>, FScheme.Value>.FromConverter(
                    args =>
                        convertToValue(
                            pyf(args.Select(ex => convertFromValue(ex)).ToList()))));
        }

        internal static FScheme.Value convertToValue(dynamic data)
        {
            if (data is FScheme.Value)
                return data;
            else if (data is string)
                return FScheme.Value.NewString(data);
            else if (data is double)
                return FScheme.Value.NewNumber(data);
            else if (data is IEnumerable<dynamic>)
                return FScheme.Value.NewList(Utils.SequenceToFSharpList(data));
            //else if (data is PythonFunction)
            //{
            //   return FuncContainer.MakeFunction(
            //      new FScheme.ExternFunc(
            //         args =>
            //            convertToValue(
            //               data(args.Select(ex => convertFromValue(ex)))
            //            )
            //      )
            //   );
            //}
            //else if (data is Func<dynamic, dynamic>)
            //{
            //   return Value.NewCurrent(FuncContainer.MakeContinuation(
            //      new Continuation(
            //         exp =>
            //            convertToValue(
            //               data(convertFromValue(exp))
            //            )
            //      )
            //   ));
            //}
            else
                return FScheme.Value.NewContainer(data);
        }

        internal static dynamic convertFromValue(FScheme.Value exp)
        {
            if (exp.IsList)
                return ((FScheme.Value.List)exp).Item.Select(x => convertFromValue(x)).ToList();
            else if (exp.IsNumber)
                return ((FScheme.Value.Number)exp).Item;
            else if (exp.IsString)
                return ((FScheme.Value.String)exp).Item;
            else if (exp.IsContainer)
                return ((FScheme.Value.Container)exp).Item;
            //else if (exp.IsFunction)
            //{
            //   return new Func<IList<dynamic>, dynamic>(
            //      args =>
            //         ((Value.Function)exp).Item
            //            .Invoke(ExecutionEnvironment.IDENT)
            //            .Invoke(Utils.convertSequence(args.Select(
            //               x => (Value)Converters.convertToValue(x)
            //            )))
            //   );
            //}
            //else if (exp.IsSpecial)
            //{
            //   return new Func<IList<dynamic>, dynamic>(
            //      args =>
            //         ((Value.Special)exp).Item
            //            .Invoke(ExecutionEnvironment.IDENT)
            //            .Invoke(
            //}
            //else if (exp.IsCurrent)
            //{
            //   return new Func<dynamic, dynamic>(
            //      ex => 
            //         Converters.convertFromValue(
            //            ((Value.Current)exp).Item.Invoke(Converters.convertToValue(ex))
            //         )
            //   );
            //}
            else
                throw new Exception("Not allowed to pass Functions into a Python Script.");
        }
    }
}
