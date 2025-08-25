package middleware

import (
	"net/http"
	"profile/internal/config"

	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
)

func RequiredRoles(cfg *config.Config, requiredRoles ...string) gin.HandlerFunc {
	roleSet := make(map[string]struct{})
	for _, r := range requiredRoles {
		roleSet[r] = struct{}{}
	}

	return func(c *gin.Context) {
		claimsAny, exists := c.Get("claims")

		if !exists {
			c.AbortWithStatusJSON(http.StatusForbidden, gin.H{"error": "Missing claims"})
			return
		}
		claims := claimsAny.(jwt.MapClaims)
		rolesRaw, ok := claims[cfg.API.Claims_role]
		if !ok {
			c.AbortWithStatusJSON(http.StatusForbidden, gin.H{"error": "No roles in token"})
			return
		}

		roles, ok := rolesRaw.(string)
		if !ok {
			c.AbortWithStatusJSON(http.StatusForbidden, gin.H{"error": "Invalid roles format"})
			return
		}
		if _, found := roleSet[roles]; found {
			c.Next()
			return
		}

		c.AbortWithStatusJSON(http.StatusForbidden, gin.H{"error": "Insufficient role"})
	}
}
