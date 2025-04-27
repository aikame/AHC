from django.utils import timezone
from django.urls import path, include
import pytz
from hello import views
from django.conf import settings
from django.conf.urls.static import static

urlpatterns = [
    path("", views.home, name="home"),
    path("settings", views.preferences, name="settings"),
    path('employee/<str:id>/img_upload', views.img_upload, name='avatar_upload'),
    path("employee/<str:id>", views.employee, name="employee"),
    path("img_upload/<str:id>", views.img_upload, name="img_upload"),
    path("computers", views.computer, name="computer"),
    path("computer/<str:id>", views.computer_detail, name="computer"),
    path("groups", views.groups, name="groups"),
    path("group/<str:id>", views.group_detail, name="group"),
    path("search/<str:location>/<str:text>", views.search, name="search"),
    path("users", views.searchall, name="search"),
    path("users/", views.searchall, name="search"),
    path("accounts/login/", views.login_page, name="login"),
    path("AD/<str:domain>/<str:id>",views.active_directory,name="AD"),
    path("auth", views.auth, name="auth"),
    path("updateComputerStatus/<str:id>", views.update_computer_status, name="upd"),
    path("createAD/<str:domain>/<str:id>",views.createAD,name="createAD"),
    path("createProfile",views.create_profile,name="createProfile"),
    path("createGroup",views.create_group,name="createGroup"),
    path("AD/<str:domain>/<str:id>/showMail",views.showMail,name="showMail"),
    path("AD/<str:domain>/<str:id>/hideMail",views.hideMail,name="hideMail"),
    path("AD/<str:domain>/<str:id>/ban",views.ban,name="ban"),
    path("AD/<str:domain>/<str:id>/unban",views.unban,name="unban"),
    path("AD/<str:domain>/<str:id>/addToGroup",views.addToGroup,name="addToGroup"),
    path("AD/<str:domain>/<str:id>/removeFromGroup",views.removeFromGroup,name="removeFromGroup"),
    path("AD/<str:domain>/<str:id>/changePassword",views.changePassword,name="changePassword"),
    #static(settings.MEDIA_URL, document_root=settings.MEDIA_ROOT)
]
