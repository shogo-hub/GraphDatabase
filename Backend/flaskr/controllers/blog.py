from flask import Blueprint, render_template

blog_bp = Blueprint("blog", __name__, url_prefix="/")


@blog_bp.route("/")
def index():
    return render_template("blog/index.html")


@blog_bp.route("/create")
def create():
    return render_template("blog/create.html")


@blog_bp.route("/<int:id>/update")
def update(id):
    return render_template("blog/update.html")
