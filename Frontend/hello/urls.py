from django.urls import path, include
from hello import views
from django.conf import settings
from django.conf.urls.static import static

urlpatterns = [
    path("", views.home, name="home"),
    path("settings", views.settings, name="settings"),
    path("employee/<str:id>", views.employee, name="employee"),
    path("img_upload/<str:id>", views.img_upload, name="img_upload"),
    path("computer", views.computer, name="computer"),
    path("computer/<str:id>", views.computer_detail, name="computer"),
    path("search/<str:text>", views.search, name="search"),
    path("search", views.searchall, name="search"),
    path("search/", views.searchall, name="search"),
    path("accounts/login/", views.login_page, name="login"),
    path("AD/<str:domain>/<str:id>",views.active_directory,name="AD"),
    path("auth", views.auth, name="auth"),
    path("updateComputerStatus/<str:id>", views.update_computer_status, name="upd")
    #static(settings.MEDIA_URL, document_root=settings.MEDIA_ROOT)
]