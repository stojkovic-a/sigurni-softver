package middleware

import (
	"net/http"
	"profile/internal/config"

	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
)

func GetId(cfg *config.Config) gin.HandlerFunc {
	return func(c *gin.Context) {
		claimsAny, exists := c.Get("claims")

		if !exists {
			c.AbortWithStatusJSON(http.StatusForbidden, gin.H{"error": "Missing claims"})
			return
		}
		claims := claimsAny.(jwt.MapClaims)
		idRaw, ok := claims[cfg.API.Claims_id]
		if !ok {
			c.AbortWithStatusJSON(http.StatusForbidden, gin.H{"error": "No id claim in token"})
			return
		}
		id := idRaw.(string)
		c.Set("id", id)
		c.Next()
	}
}
