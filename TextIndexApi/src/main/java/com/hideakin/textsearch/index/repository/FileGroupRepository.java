package com.hideakin.textsearch.index.repository;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.FileGroupEntity;

@Repository
public interface FileGroupRepository extends JpaRepository<FileGroupEntity,Integer> {

	FileGroupEntity findByGid(int gid);
	FileGroupEntity findByName(String name);

}
