# Lilac #
----------
Lilac is a functional programming language, inspired primarily by Scheme. My goal is to create a language that fully supports functional and object-oriented styles, and that has friendly syntax and semantics.

Here is a small program, adapted from the sample program for Emily at [emilylang.org](http://emilylang.org):

	open "list.li"

    let for upto perform = 
        let iter i =
            if i < upto then
                perform i
                iter (i + 1)
        iter 0

    let create-line old =
        let line = []
        let final = old.length - 1
        for old.length lambda i =
            let here = old.at i
            let before = old.at (if i = 0 then final else i - 1)
            let after = old.at (if i = final then 0 else i + 1)
            line.add! ((here and before and after) or not (here or before or after))
        line

    let print-line line = 
        foreach (lambda i = if i then print "*" else print " ") line
        println ""

    let ref current-line = []
    let ref next = 1

    for 80 lambda i =
        current-line.add! (i != next)
        if i = next then set! next = next * 2

    for 30 lambda _ =
        print-line current-line
        set! current-line = create-line current-line

This prints the [Rule 135 cellular automaton](https://en.wikipedia.org/wiki/Rule_30).

    *  * *** ******* *************** ******************************* ***************
          *   *****   *************   *****************************   **************
     ****   *  ***  *  ***********  *  ***************************  *  ************
      **  *     *       *********       *************************       **********
    *       ***   *****  *******  *****  ***********************  *****  ********  *
      *****  *  *  ***    *****    ***    *********************    ***    ******
    *  ***          *  **  ***  **  *  **  *******************  **  *  **  ****  ***
        *  ********         *               *****************               **    **
     **     ******  *******   *************  ***************  *************    **
        ***  ****    *****  *  ***********    *************    ***********  **    **
     **  *    **  **  ***       *********  **  ***********  **  *********      **
           **          *  *****  *******        *********        *******  ****    **
     *****    ********     ***    *****  ******  *******  ******  *****    **  **
      ***  **  ******  ***  *  **  ***    ****    *****    ****    ***  **        **
       *        ****    *           *  **  **  **  ***  **  **  **  *      ******
    **   ******  **  **   *********                 *                 ****  ****  **
    *  *  ****          *  *******  ***************   ***************  **    **    *
           **  ********     *****    *************  *  *************      **    **
    ******      ******  ***  ***  **  ***********       ***********  ****    **    *
    *****  ****  ****    *    *        *********  *****  *********    **  **    **
     ***    **    **  **   **   ******  *******    ***    *******  **        **
      *  **    **        *    *  ****    *****  **  *  **  *****      ******    ****
            **    ******   **     **  **  ***               ***  ****  ****  **  **
    *******    **  ****  *    ***          *  *************  *    **    **
     *****  **      **     **  *  ********     ***********     **    **    ********
      ***      ****    ***         ******  ***  *********  ***    **    **  ******
    *  *  ****  **  **  *  *******  ****    *    *******    *  **    **      ****  *
           **               *****    **  **   **  *****  **       **    ****  **
    ******    *************  ***  **        *      ***      *****    **  **      ***
    *****  **  ***********    *      ******   ****  *  ****  ***  **        ****  **

The syntax is clean and minimal. Code is grouped into blocks with parentheses and indentation. Variables are bound and functions are defined with the `let` keyword. Functions are applied simply by writing the arguments after the name of the function. Using `let ref` creates a mutable binding that can be reassigned with the `set!` keyword. Anonymous function objects can be created with the `lambda` keyword. Object members are accessed by using a period. By convention, identifiers should use kebab-case, and functions or keywords that mutate objects should be suffixed with a `!`. The language currently also supports namespacing, with the `namespace` and `using` keywords, and namespace resolution is also done by using periods. Users can also define their own binary infix operators using `let operator`.

Internally, all functions are treated as unary and are automatically curried. Nullary functions are defined by using `let func () = ...`. They simply discard the argument they are applied to, and by convention should be called by passing them `()`, the empty group, which evaluates to the `unit` singleton.

Lilac is still very bare-bones, and there are a lot of features I would like to add moving forward:

- Support for tail recursion
- Gradual strong typing
- Support for inheritance and polymorphism
- Explicit support for generic functions
- User defined classes and interfaces 
- User defined syntax transformations
- Libraries for common tasks like regular expressions and file i/o
- JIT compilation
