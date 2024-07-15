from django.urls import path, include
from hello import views

urlpatterns = [
    path("", views.home, name="home"),
    path("settings", views.settings, name="settings"),
    path("employee/<str:id>", views.employee, name="employee"),
    path("computer", views.computer, name="computer"),
    path("search/<str:text>", views.search, name="search"),
    path("search", views.searchall, name="search"),
    path("search/", views.searchall, name="search"),
    path("login", views.login, name="login"),
    path("AD/<str:domain>/<str:id>",views.active_directory,name="AD"),
]