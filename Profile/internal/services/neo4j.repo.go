package services

import "profile/internal/models"

type Neo4jRepo interface {
	Close() error
	CreateProfile(*models.Profile) error
	DeleteProfile(*string) error
	AddFriend(*string, *string) error
	Unfriend(*string, *string) error
	GetProfileById(*string) (*models.Profile, error)
	GetProfileByUsername(*string) (*models.Profile, error)
}
