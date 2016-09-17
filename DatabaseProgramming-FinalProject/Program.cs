/*
 * John Fox - MIs 218 Summer 2016 - Final Project: Database Application
 * 
 * This was a pretty good project to end things on. It was a head-scratcher
 * in many respects, and I went down a couple blind alleys trying to figure
 * some things out, but it never felt like an unfair assignment.
 * 
 * I enjoyed getting a chance to use some of the SQL knowledge 
 * I acquired 1.5 years ago, especially constraints. I'd sort
 * of forgotten how cool and helpful they can be.
 * 
 * I do wish our book/class had covered parameterized
 * SQL, as that's the right real-world way of doing 
 * these things, and I would have preferred it to
 * using string literals. But that's a pretty minor
 * gripe, and I'm not going to ding you any points
 * for it. ;)
 * 
 * Thank you, John. I've enjoyed my classes with you.
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
 
namespace DatabaseProgramming_FinalProject {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
