package main

import (
	"fmt"
	"log"
	"profile/internal/api/controllers"
	"profile/internal/config"
	"profile/internal/grpcclients"
	"profile/internal/middleware"
	"profile/internal/models"
	"profile/internal/services"

	"github.com/gin-gonic/gin"
	"google.golang.org/grpc"

	docs "profile/cmd/profile/docs"

	swaggerFiles "github.com/swaggo/files"
	ginSwagger "github.com/swaggo/gin-swagger"
)

var (
	server            *gin.Engine
	neoRepo           services.Neo4jRepo
	profileService    services.ProfileService
	authClient        grpcclients.AuthClient
	conn              *grpc.ClientConn
	cfg               *config.Config
	profileController controllers.ProfileController
)

func init() {
	cfg = config.Load()
	neoRepo = services.NewNeo4jService(cfg)
	profileService = services.NewProfileService(cfg, &neoRepo)
	authClient, conn = grpcclients.NewAuthClient(fmt.Sprintf("%s:%d", cfg.Grpc.Server, cfg.Grpc.Port), cfg)
	profileController = controllers.NewProfileContoller(profileService, authClient, cfg)
	server = gin.Default()
	if err := server.SetTrustedProxies(nil); err != nil {
		log.Fatalf("failed to set trusted proxies: %v", err)
	}
	docs.SwaggerInfo.BasePath = "/v1"
}

// @securityDefinitions.apikey BearerAuth
// @in header
// @name Authorization
// @description Type "Bearer" followed by a space and JWT token.
func main() {
	defer conn.Close()
	defer profileService.CloseDbConnection()

	public := server.Group("/v1/public")
	profileController.RegisterPublicProfileRoutes(public)

	protected := server.Group("/v1/protected")
	// protected.Use(middleware.JWTAuthMiddleware(cfg))
	protected.Use(middleware.JWTRemoteAuthMiddleware(&authClient))
	profileController.RegisterProtectedProfileRoutes(protected)

	internal := server.Group("/v1")
	internal.Use(middleware.JWTRemoteAuthMiddleware(&authClient))
	internal.Use(middleware.RequiredRoles(cfg, models.ADMIN))
	profileController.RegisterInternalProfileRoutes(internal)

	server.GET("/swagger/*any", ginSwagger.WrapHandler(swaggerFiles.Handler, ginSwagger.DocExpansion("none")))
	log.Fatal(server.RunTLS(fmt.Sprintf("%s:%d", cfg.API.Server, cfg.API.Port), cfg.API.Client_cert_crt, cfg.API.Client_cert_key))
}
