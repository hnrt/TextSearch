package com.hideakin.textsearch.index.repository;

import java.util.List;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.FileEntity;

@Repository
public interface FileRepository extends JpaRepository<FileEntity,Integer> {

	public FileEntity findByFid(int fid);
	public List<FileEntity> findAllByPath(String path);
	public List<FileEntity> findAllByGid(int gid);

}
