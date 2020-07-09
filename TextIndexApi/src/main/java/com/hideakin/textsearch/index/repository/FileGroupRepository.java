package com.hideakin.textsearch.index.repository;

import javax.persistence.LockModeType;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Lock;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.FileGroupEntity;

@Repository
public interface FileGroupRepository extends JpaRepository<FileGroupEntity,Integer> {

	FileGroupEntity findByGid(int gid);

	FileGroupEntity findByName(String name);

	@Lock(LockModeType.PESSIMISTIC_WRITE)
	@Query("SELECT g FROM file_groups g WHERE g.gid = :gid")
	FileGroupEntity findByGidForUpdate(@Param("gid") int gid);

	@Lock(LockModeType.PESSIMISTIC_WRITE)
	@Query("SELECT g FROM file_groups g WHERE g.name = :name")
	FileGroupEntity findByNameForUpdate(@Param("name") String name);

}
