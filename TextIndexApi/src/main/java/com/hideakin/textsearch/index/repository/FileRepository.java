package com.hideakin.textsearch.index.repository;

import java.util.List;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.FileEntity;

@Repository
public interface FileRepository extends JpaRepository<FileEntity,Integer> {

	FileEntity findByFid(int fid);
	List<FileEntity> findAllByGidAndPath(int gid, String path);
	List<FileEntity> findAllByGidAndPathAndStaleFalse(int gid, String path);
	List<FileEntity> findAllByGid(int gid);
	List<FileEntity> findAllByGidAndStaleTrue(int gid);
	List<FileEntity> findAllByGidAndStaleFalse(int gid);
	void deleteByFid(int fid);
	void deleteByGid(int gid);

}
