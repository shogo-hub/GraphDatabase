from flask import Blueprint, render_template, request, redirect, url_for, flash

auth_bp = Blueprint("auth", __name__, url_prefix="/auth")


@auth_bp.route("/register", methods=("GET", "POST"))
def register():
    if request.method == "POST":
        # TODO: implement registration logic (validation, create user)
        flash("Registered (placeholder)")
        return redirect(url_for("auth.login"))
    return render_template("auth/register.html")


@auth_bp.route("/login", methods=("GET", "POST"))
def login():
    if request.method == "POST":
        # TODO: implement authentication
        flash("Logged in (placeholder)")
        return redirect(url_for("blog.index"))
    return render_template("auth/login.html")


@auth_bp.route("/logout")
def logout():
    # TODO: implement logout
    flash("Logged out (placeholder)")
    return redirect(url_for("blog.index"))
