package middleware

import (
	"log"
	"net/http"
	"profile/internal/config"
	"profile/internal/grpcclients"
	"strconv"
	"strings"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
)

func JWTAuthMiddleware(cfg *config.Config) gin.HandlerFunc {
	return func(c *gin.Context) {
		authHeader := c.GetHeader("Authorization")
		if authHeader == "" || !strings.HasPrefix(authHeader, "Bearer") {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "Authorization header missing or malformed"})
			return
		}

		tokenString := strings.TrimPrefix(authHeader, "Bearer ")

		secret := cfg.API.Authentication_secret
		if secret == "" {
			log.Println("JWT_SIGNING_KEY not set")
			c.AbortWithStatus(http.StatusInternalServerError)
			return
		}

		token, err := jwt.Parse(tokenString, func(token *jwt.Token) (interface{}, error) {
			if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
				return nil, jwt.ErrSignatureInvalid
			}
			return []byte(secret), nil
		})

		if err != nil {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "Invalid token: " + err.Error()})
			return
		}

		if !token.Valid {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "Token is not valid"})
			return
		}
		if claims, ok := token.Claims.(jwt.MapClaims); ok {
			if expRaw, exists := claims["exp"]; exists {
				exp := int64(expRaw.(float64))
				if time.Now().Unix() > exp {
					c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "Token expired"})
					return
				}
			}
			// Store claims for handlers
			c.Set("claims", claims)
		}

		c.Next()

	}
}

func JWTRemoteAuthMiddleware(authClient *grpcclients.AuthClient) gin.HandlerFunc {
	return func(c *gin.Context) {
		authHeader := c.GetHeader("Authorization")
		if authHeader == "" || !strings.HasPrefix(authHeader, "Bearer") {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "Authorization header missing or malformed"})
			return
		}

		tokenString := strings.TrimPrefix(authHeader, "Bearer ")

		validationReply, err := (*authClient).ValidateToken(tokenString)
		if err != nil {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "Failed to validate a token: " + err.Error()})
			return
		}

		if !validationReply.IsValid {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "Failed to validate a token: " + *validationReply.Error})
			return
		}
		claims := jwt.MapClaims{}
		for key, value := range validationReply.Claims {
			claims[key] = value
		}
		if expRaw, exists := claims["exp"]; exists {
			unixTime := expRaw.(string)
			exp, err := strconv.ParseInt(unixTime, 10, 64)
			if err != nil {
				c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "Invalid expirey structure"})
			}
			if time.Now().Unix() > exp {
				c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "Token expired"})
				return
			}
		}
		c.Set("claims", claims)
		c.Next()
	}
}
