// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function onSubmit(e) {
    //e.preventDefault();
    alert('hello');

    grecaptcha.ready(function () {
        grecaptcha.execute('6LcC9IUqAAAAAL6JE7blweCaalkMn2hCtyWUlWIh', { action: 'submit' }).then(function (token) {
            // Add your logic to submit to your backend server here.
        });
    });
}