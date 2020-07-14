package com.hideakin.textsearch.index.controller;

import java.util.List;
import java.util.Map;

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
import com.hideakin.textsearch.index.model.ObjectDisposition;
import com.hideakin.textsearch.index.model.FileInfo;
import com.hideakin.textsearch.index.model.FileStats;
import com.hideakin.textsearch.index.service.FileService;
import com.hideakin.textsearch.index.utility.ContentType;
import com.hideakin.textsearch.index.utility.GZipHelper;
import com.hideakin.textsearch.index.utility.TextEncoding;
import com.hideakin.textsearch.index.utility.TextTokenizer;

@RestController
public class FileController {

	@Autowired
	private FileService service;

	@RequestMapping(value="/v1/files/{group:[^0-9].*}",method=RequestMethod.GET)
	public ResponseEntity<?> getFiles(
			@PathVariable String group) {
		FileInfo[] infoArray = service.getFiles(group);
		if (infoArray != null) {
			return new ResponseEntity<>(infoArray, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/files/{group:[^0-9].*}/stats",method=RequestMethod.GET)
	public ResponseEntity<?> getFileStats(
			@PathVariable String group) {
		FileStats stats = service.getStats(group);
		if (stats != null) {
			return new ResponseEntity<>(stats, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/files/{group:[^0-9].*}/file",method=RequestMethod.GET)
	public ResponseEntity<?> getFileByPath(
			@PathVariable String group,
			@RequestParam("path") String path) {
		FileInfo info = service.getFile(group, path);
		if (info != null) {
			return new ResponseEntity<>(info, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/files/{fid:[0-9]+}",method=RequestMethod.GET)
	public ResponseEntity<?> getFile(
			@PathVariable int fid) {
		FileInfo info = service.getFile(fid);
		if (info != null) {
			return new ResponseEntity<>(info, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/files/{fid:[0-9]+}/contents",method=RequestMethod.GET)
	public ResponseEntity<?> getFileContents(
			@PathVariable int fid) {
		String path = service.getPath(fid);
		byte[] contents = service.getContents(fid);
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
			byte[] textUTF8 = TextEncoding.convertToUTF8(file.getBytes(), ContentType.parse(file.getContentType()).getCharset(TextEncoding.UTF_8));
			byte[] compressed = GZipHelper.compress(textUTF8);
			TextTokenizer tokenizer = new TextTokenizer();
			tokenizer.run(textUTF8, TextEncoding.UTF_8);
			Map<String,List<Integer>> textMap = tokenizer.populateTextMap();
			ObjectDisposition disp = new ObjectDisposition();
			FileInfo added = service.addFile(group, file.getOriginalFilename(), textUTF8.length, compressed, textMap, disp);
			if (added != null) {
				return new ResponseEntity<>(added, disp.isCreated() ? HttpStatus.CREATED : HttpStatus.OK);
			} else {
				return new ResponseEntity<>(HttpStatus.NOT_FOUND);
			}
		} catch (Exception e) {
			e.printStackTrace();
			return new ResponseEntity<>(HttpStatus.INTERNAL_SERVER_ERROR);
		}
	}

	@RequestMapping(value="/v1/files/{fid:[0-9]+}/contents",method=RequestMethod.PUT,consumes=MediaType.MULTIPART_FORM_DATA_VALUE)
	public ResponseEntity<?> updateFile(
			@PathVariable int fid,
			@RequestParam("file") MultipartFile file) {
		try {
			byte[] textUTF8 = TextEncoding.convertToUTF8(file.getBytes(), ContentType.parse(file.getContentType()).getCharset(TextEncoding.UTF_8));
			byte[] compressed = GZipHelper.compress(textUTF8);
			TextTokenizer tokenizer = new TextTokenizer();
			tokenizer.run(textUTF8, TextEncoding.UTF_8);
			Map<String,List<Integer>> textMap = tokenizer.populateTextMap();
			FileInfo added = service.updateFile(fid, file.getOriginalFilename(), textUTF8.length, compressed, textMap);
			if (added != null) {
				return new ResponseEntity<>(added, HttpStatus.OK);
			} else {
				return new ResponseEntity<>(HttpStatus.NOT_FOUND);
			}
		} catch (Exception e) {
			e.printStackTrace();
			return new ResponseEntity<>(HttpStatus.INTERNAL_SERVER_ERROR);
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
