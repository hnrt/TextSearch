package com.hideakin.textsearch.index.controller;

import java.io.IOException;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.multipart.MultipartFile;

import com.hideakin.textsearch.index.entity.TextResourceHttpEntity;
import com.hideakin.textsearch.index.model.FileDisposition;
import com.hideakin.textsearch.index.model.FileInfo;
import com.hideakin.textsearch.index.model.FileStats;
import com.hideakin.textsearch.index.service.FileService;

@RestController
public class FileController {

	@Autowired
	private FileService service;

	@RequestMapping(value="/v1/files/{group:[^0-9].*}",method=RequestMethod.GET)
	public ResponseEntity<?> getFiles(
			@PathVariable String group) {
		FileInfo[] fiArray = service.getFiles(group);
		if (fiArray != null) {
			return new ResponseEntity<>(fiArray, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/files/{group:[^0-9].*}/stats",method=RequestMethod.GET)
	public ResponseEntity<?> getFileStats(
			@PathVariable String group) {
		FileStats stats = service.getFileStats(group);
		if (stats != null) {
			return new ResponseEntity<>(stats, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/files/{fid:[0-9]+}",method=RequestMethod.GET)
	public ResponseEntity<?> getFile(
			@PathVariable int fid) {
		FileInfo fi = service.getFile(fid);
		if (fi != null) {
			return new ResponseEntity<>(fi, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/files/{fid:[0-9]+}/contents",method=RequestMethod.GET)
	public ResponseEntity<?> getFileContents(
			@PathVariable int fid) {
		String path = service.getPath(fid);
		byte[] contents = service.getFileContents(fid);
		if (path != null && contents != null) {
			HttpHeaders headers = new HttpHeaders();
			headers.setContentType(MediaType.MULTIPART_FORM_DATA);
			MultiValueMap<String,Object> formData = new LinkedMultiValueMap<>();
			formData.add("file", new TextResourceHttpEntity(contents, path));
			return new ResponseEntity<>(formData, headers, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/files/{group:[^0-9].*}",method=RequestMethod.POST,consumes=MediaType.MULTIPART_FORM_DATA_VALUE)
	public ResponseEntity<?> addFile(
			@PathVariable String group,
			@RequestParam("file") MultipartFile file) {
		try {
			FileDisposition disp = new FileDisposition();
			FileInfo added = service.addFile(group, file.getOriginalFilename(), file.getBytes(), file.getContentType(), disp);
			if (added != null) {
				return new ResponseEntity<>(added, disp.isCreated() ? HttpStatus.CREATED : HttpStatus.OK);
			} else {
				return new ResponseEntity<>(HttpStatus.NOT_FOUND);
			}
		} catch (IOException e) {
			e.printStackTrace();
			return new ResponseEntity<>(HttpStatus.BAD_REQUEST);
		}
	}

	@RequestMapping(value="/v1/files/{fid:[0-9]+}/contents",method=RequestMethod.PUT,consumes=MediaType.MULTIPART_FORM_DATA_VALUE)
	public ResponseEntity<?> updateFile(
			@PathVariable int fid,
			@RequestParam("file") MultipartFile file) {
		try {
			FileInfo added = service.updateFile(fid, file.getOriginalFilename(), file.getBytes(), file.getContentType());
			if (added != null) {
				return new ResponseEntity<>(added, HttpStatus.OK);
			} else {
				return new ResponseEntity<>(HttpStatus.NOT_FOUND);
			}
		} catch (IOException e) {
			e.printStackTrace();
			return new ResponseEntity<>(HttpStatus.BAD_REQUEST);
		}
	}

	@RequestMapping(value="/v1/files/{group:[^0-9].*}",method=RequestMethod.DELETE)
	public ResponseEntity<?> deleteFiles(
			@PathVariable String group) {
		FileInfo[] deleted = service.deleteFiles(group);
		if (deleted != null) {
			return new ResponseEntity<>(deleted, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/files/{group:[^0-9].*}/stale",method=RequestMethod.DELETE)
	public ResponseEntity<?> deleteStaleFiles(
			@PathVariable String group) {
		if (service.deleteStaleFiles(group)) {
			return new ResponseEntity<>(HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/files/{fid:[0-9]+}",method=RequestMethod.DELETE)
	public ResponseEntity<?> deleteFile(
			@PathVariable int fid) {
		FileInfo deleted = service.deleteFile(fid);
		if (deleted != null) {
			return new ResponseEntity<>(deleted, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

}
