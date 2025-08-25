package controllers

import (
	"fmt"
	"net/http"
	"profile/internal/config"
	"profile/internal/grpcclients"
	"profile/internal/middleware"
	"profile/internal/models"
	"profile/internal/services"

	"github.com/gin-gonic/gin"
)

type ProfileController struct {
	ProfileService services.ProfileService
	authClient     grpcclients.AuthClient
	cfg            *config.Config
}

func NewProfileContoller(profileService services.ProfileService, authClient grpcclients.AuthClient, cfg *config.Config) ProfileController {
	return ProfileController{
		ProfileService: profileService,
		authClient:     authClient,
		cfg:            cfg,
	}
}

// Summary Deletes the user's profile
// @Produce json
// @Success 200
// @Security BearerAuth
// @Router /protected/deleteProfile [delete]
func (e *ProfileController) DeleteProfile(ctx *gin.Context) {
	id, _ := ctx.Get("id")

	idString := id.(string)
	if err := e.ProfileService.DeleteProfile(&idString); err != nil {
		ctx.JSON(http.StatusInternalServerError, fmt.Sprintf("Failed to delete a profile: %v", err))
		return
	}
	ctx.JSON(200, "Profile successfully deleted.")
}

// Summary Creates a friendship between current user and user given by id
// @Produce json
// @Success 200
// @Security BearerAuth
// @Router /protected/addFriend [post]
func (e *ProfileController) AddFriend(ctx *gin.Context) {
	id, _ := ctx.Get("id")
	idString := id.(string)
	var friendId string
	if err := ctx.ShouldBindJSON(&friendId); err != nil {
		ctx.JSON(http.StatusBadRequest, "No parameters")
		return
	}

	if err := e.ProfileService.AddFriend(&idString, &friendId); err != nil {
		ctx.JSON(http.StatusInternalServerError, fmt.Sprintf("Failed to add a friend %v", err))
		return
	}
	ctx.JSON(200, "Friendship successfully created.")

}

// Summary Creates a profile entity in database
// @Produce json
// @Success 200
// @Security BearerAuth
// @Router /internal/createProfile [post]
func (e *ProfileController) CreateProfile(ctx *gin.Context) {
	var profile models.Profile
	if err := ctx.ShouldBindJSON(&profile); err != nil {
		ctx.JSON(http.StatusBadRequest, "No parameters")
		return
	}

	if err := e.ProfileService.CreateProfile(&profile); err != nil {
		ctx.JSON(http.StatusInternalServerError, fmt.Sprintf("Failed to create a profile %v", err))
		return
	}
	ctx.JSON(200, "Profile created.")

}

// Summary Deletes a friendship between current user and user given by id
// @Produce json
// @Success 200
// @Security BearerAuth
// @Router /protected/unfriend [post]
func (e *ProfileController) Unfriend(ctx *gin.Context) {
	id, _ := ctx.Get("id")
	idString := id.(string)
	var friendId string
	if err := ctx.ShouldBindJSON(&friendId); err != nil {
		ctx.JSON(http.StatusBadRequest, "No parameters")
		return
	}

	if err := e.ProfileService.Unfriend(&idString, &friendId); err != nil {
		ctx.JSON(http.StatusInternalServerError, fmt.Sprintf("Failed to unfriend: %v", err))
		return
	}
	ctx.JSON(200, "Friendship successfully deleted.")

}

type GetProfileByIdQuery struct {
	id string `form:"id"`
}

// Summary Returns a profile by id
// @Produce json
// @Success 200
// @Security BearerAuth
// @Router /protected/getProfileById [get]
func (e *ProfileController) GetProfileById(ctx *gin.Context) {
	var query GetProfileByIdQuery
	if err := ctx.ShouldBindQuery(&query); err != nil {
		ctx.JSON(http.StatusBadRequest, "No query parameters")
		return
	}
	profile, err := e.ProfileService.GetProfileById(&query.id)
	if err != nil {
		ctx.JSON(http.StatusNotFound, fmt.Sprintf("User with id: %s not found", query.id))
		return
	}
	ctx.JSON(200, profile)
}

// Summary Returns a profile by username
// @Produce json
// @Success 200
// @Security BearerAuth
// @Router /protected/getProfileByUsername/{username} [get]
func (e *ProfileController) GetProfileByUsername(ctx *gin.Context) {
	username := ctx.Param("username")
	profile, err := e.ProfileService.GetProfileByUsername(&username)
	if err != nil {
		ctx.JSON(http.StatusNotFound, fmt.Sprintf("User with username: %s not found", username))
		return
	}
	ctx.JSON(200, profile)
}

// Summary Returns "test"
// @Produce json
// @Success 200
// @Security BearerAuth
// @Router /protected/test [get]
func (e *ProfileController) Test(ctx *gin.Context) {
	temp, er := e.authClient.Test("asd")
	if er != nil {
		ctx.JSON(400, er)
	}
	ctx.JSON(200, temp.OtherGield)
}

// Summary Returns "test"
// @Produce json
// @Success 200
// @Router /public/testPublic [get]
func (e *ProfileController) TestPublic(ctx *gin.Context) {
	temp, er := e.authClient.Test("asd")
	if er != nil {
		ctx.JSON(400, er)
	}
	ctx.JSON(200, temp.OtherGield)
}

func (e *ProfileController) RegisterInternalProfileRoutes(rg *gin.RouterGroup) {
	internalRoute := rg.Group("/internal")
	internalRoute.POST("/createProfile", e.CreateProfile)
}
func (e *ProfileController) RegisterProtectedProfileRoutes(rg *gin.RouterGroup) {
	// profileRoute := rg.Group("/profile")
	rg.GET("/test", e.Test)
	rg.DELETE("/deleteProfile", middleware.RequiredRoles(e.cfg, models.USER), middleware.GetId(e.cfg), e.DeleteProfile)
	rg.POST("/addFriend", middleware.RequiredRoles(e.cfg, models.USER), middleware.GetId(e.cfg), e.AddFriend)
	rg.PUT("/unfirend", middleware.RequiredRoles(e.cfg, models.USER), middleware.GetId(e.cfg), e.Unfriend)
	rg.GET("/getProfileById", middleware.RequiredRoles(e.cfg, models.USER), e.GetProfileById)
	rg.GET("/getProfileUsername", middleware.RequiredRoles(e.cfg, models.USER), e.GetProfileByUsername)

}

func (e *ProfileController) RegisterPublicProfileRoutes(rg *gin.RouterGroup) {
	// profileRoute := rg.Group("/profile")
	rg.GET("/testPublic", e.TestPublic)
}
